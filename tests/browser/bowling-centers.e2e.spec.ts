import { test, expect, Page } from '@playwright/test';

const mockCenters = [
  {
    name: 'Central Lanes',
    street: '123 Main St',
    unit: '',
    city: 'Boston',
    state: 'MA',
    zipCode: '02108',
    phoneNumber: '16175550123',
    phoneExtension: '',
    latitude: 42.3601,
    longitude: -71.0589,
    isClosed: false
  },
  {
    name: 'Harbor Bowl',
    street: '45 Ocean Ave',
    unit: '',
    city: 'New Haven',
    state: 'CT',
    zipCode: '06510',
    phoneNumber: '12035550123',
    phoneExtension: '',
    latitude: 41.3083,
    longitude: -72.9279,
    isClosed: false
  },
  {
    name: 'Pine Tree Lanes',
    street: '9 Forest Rd',
    unit: 'Suite 200',
    city: 'Portland',
    state: 'ME',
    zipCode: '04101',
    phoneNumber: '12075551234',
    phoneExtension: '',
    latitude: 43.6591,
    longitude: -70.2568,
    isClosed: false
  }
];

const mapModuleStub = `
export async function initializeMap(authConfig, mapConfig, locations, dotNetRef) {
  globalThis.azureMapsAuthConfig = authConfig;
  if (dotNetRef?.invokeMethodAsync) {
    await dotNetRef.invokeMethodAsync('NotifyBoundsChanged', { south: 40, west: -74, north: 45, east: -69 });
  }
}
export async function updateMarkers() {}
export async function focusOnLocation() {}
export async function fitBounds() {}
export async function closePopup() {}
export async function enterDirectionsPreview() {}
export async function showRoute() {
  return {
    DistanceMeters: 16093.4,
    TravelTimeSeconds: 900,
    Instructions: [
      { Text: 'Head north on Main St', DistanceMeters: 5000 },
      { Text: 'Arrive at destination', DistanceMeters: 100 }
    ],
    RouteGeoJson: '{}'
  };
}
export async function exitDirectionsMode() {}
export async function setMapStyle() {}
export async function dispose() {}
`;

const directionsModuleStub = `
export async function getCurrentLocation() {
  return [-71.0589, 42.3601];
}
export async function searchAddress(query) {
  if (!query) return [];
  return [
    { Address: '123 Main St, Boston, MA', Locality: 'Boston, MA', Latitude: 42.3601, Longitude: -71.0589 },
    { Address: '45 Ocean Ave, New Haven, CT', Locality: 'New Haven, CT', Latitude: 41.3083, Longitude: -72.9279 }
  ];
}
export function openInNewTab() {}
`;

async function stubMapModules(page: Page) {
  await page.route('**/Maps/NebaMap.razor.js', route =>
    route.fulfill({ status: 200, contentType: 'application/javascript', body: mapModuleStub })
  );
  await page.route('**/BowlingCenters/DirectionsModal.razor.js', route =>
    route.fulfill({ status: 200, contentType: 'application/javascript', body: directionsModuleStub })
  );
  await page.route('https://atlas.microsoft.com/sdk/javascript/mapcontrol/**', route =>
    route.fulfill({ status: 200, contentType: 'application/javascript', body: '' })
  );
  await page.route('https://atlas.microsoft.com/sdk/javascript/mapcontrol/3/atlas.min.css', route =>
    route.fulfill({ status: 200, contentType: 'text/css', body: '' })
  );
}

async function mockBowlingCenters(page: Page, options?: { centers?: typeof mockCenters; status?: number }) {
  const centers = options?.centers ?? mockCenters;
  const status = options?.status ?? 200;

  await page.route('**/bowling-centers', route => {
    if (status !== 200) {
      return route.fulfill({ status, contentType: 'application/json', body: JSON.stringify({ error: 'Server error' }) });
    }

    return route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ items: centers })
    });
  });
}

async function visitCenters(page: Page, options?: { centers?: typeof mockCenters; status?: number }) {
  await stubMapModules(page);
  await mockBowlingCenters(page, options);
  await page.goto('/bowling-centers');
}

test.describe('Bowling Centers Page - E2E User Journeys', () => {
  test('renders centers and displays counts after load', async ({ page }) => {
    await visitCenters(page);

    const cards = page.getByTestId('center-card');
    await expect(cards.first()).toBeVisible({ timeout: 5000 });
    await expect(cards).toHaveCount(mockCenters.length);

    await expect(page.getByTestId('results-count')).toContainText('Showing 3 of 3 centers');
  });

  test('filters centers by state and updates results count', async ({ page }) => {
    await visitCenters(page);

    await page.getByTestId('state-filter-CT').click();

    await expect(page.getByTestId('center-card')).toHaveCount(1);
    await expect(page.getByTestId('center-card').first()).toContainText('Harbor Bowl');
    await expect(page.getByTestId('results-count')).toContainText('Showing 1 of 1 centers');
  });

  test('search narrows the displayed centers list', async ({ page }) => {
    await visitCenters(page);

    await page.getByTestId('center-search-input').fill('Pine');

    const cards = page.getByTestId('center-card');
    await expect(cards).toHaveCount(1);
    await expect(cards.first()).toContainText('Pine Tree Lanes');
    await expect(page.getByTestId('results-count')).toContainText('Showing 1 of 3 centers');
  });

  test('directions modal shows route summary after using current location', async ({ page }) => {
    await visitCenters(page);

    await page.getByTestId('directions-btn').first().click();
    await expect(page.getByTestId('directions-modal-body')).toBeVisible();

    await page.getByTestId('use-location-btn').click();

    const summary = page.getByTestId('route-summary');
    await expect(summary).toBeVisible();
    await expect(summary).toContainText('10.0 mi');
    await expect(summary).toContainText('15 min');

    await page.getByText('Turn-by-turn directions', { exact: false }).click();
    await expect(page.getByTestId('directions-steps')).toBeVisible();
  });

  test('map style toggle reflects selected style', async ({ page }) => {
    await visitCenters(page);

    const satelliteButton = page.getByTestId('map-style-satellite');
    const roadButton = page.getByTestId('map-style-road');

    await satelliteButton.click();
    await expect(satelliteButton).toHaveClass(/bg-\[var\(--neba-blue-600\)\]/);
    await expect(roadButton).not.toHaveClass(/bg-\[var\(--neba-blue-600\)\]/);

    const hybridButton = page.getByTestId('map-style-hybrid');
    await hybridButton.click();
    await expect(hybridButton).toHaveClass(/bg-\[var\(--neba-blue-600\)\]/);
  });

  test('shows an error alert when the API call fails', async ({ page }) => {
    await visitCenters(page, { status: 500 });

    await expect(page.getByText('Error Loading Centers')).toBeVisible();
    await expect(page.getByText('Failed to load bowling centers', { exact: false })).toBeVisible();
  });
});
