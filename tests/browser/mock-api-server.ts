import { createServer, IncomingMessage, ServerResponse } from 'http';

const MOCK_DOCUMENT_HTML = `
<div>
  <h1 id="section-10.3-annual-meeting">Section 10.3 Annual Meeting</h1>
  <p>The annual meeting shall be held...</p>
  <p>See also <a href="#established-election-cycle">established election cycle</a> and <a href="#quorum-provisions">quorum provisions</a>.</p>
  <p>For more details, refer to <a href="#section-12.1-amendments">amendments</a> and <a href="#article-vii-hall-of-fame-committee">Hall of Fame Committee</a>.</p>

  <h2 id="article-vii-hall-of-fame-committee">ARTICLE VII - HALL OF FAME COMMITTEE</h2>
  <p>The Hall of Fame Committee shall...</p>
  <p>Related: <a href="#section-10.3-annual-meeting">Annual Meeting</a></p>

  <h2 id="established-election-cycle">Established Election Cycle</h2>
  <p>Elections shall be conducted on a regular cycle...</p>

  <h2 id="quorum-provisions">Quorum Provisions</h2>
  <p>A quorum shall consist of...</p>

  <h2 id="section-12.1-amendments">Section 12.1 Amendments</h2>
  <p>These bylaws may be amended...</p>

  <!-- Link to another document -->
  <p>See <a href="/tournaments/rules">Tournament Rules</a> for competition guidelines.</p>
</div>
`;

const MOCK_BOWLING_CENTERS = {
  items: [
    {
      name: 'Boston Bowl',
      street: '820 Morrissey Blvd',
      unit: null,
      city: 'Boston',
      state: 'MA',
      zipCode: '02122',
      phoneNumber: '16172888600',
      phoneExtension: null,
      latitude: 42.2875,
      longitude: -71.0483,
      isClosed: false
    },
    {
      name: 'Kings Bowl Burlington',
      street: '60 Middlesex Turnpike',
      unit: null,
      city: 'Burlington',
      state: 'MA',
      zipCode: '01803',
      phoneNumber: '17812709695',
      phoneExtension: null,
      latitude: 42.4876,
      longitude: -71.2032,
      isClosed: false
    },
    {
      name: 'Spare Time Worcester',
      street: '106 Shrewsbury Street',
      unit: null,
      city: 'Worcester',
      state: 'MA',
      zipCode: '01604',
      phoneNumber: '15087991000',
      phoneExtension: null,
      latitude: 42.2626,
      longitude: -71.8023,
      isClosed: false
    },
    {
      name: 'Holiday Lanes',
      street: '150 Main Street',
      unit: null,
      city: 'Manchester',
      state: 'CT',
      zipCode: '06040',
      phoneNumber: '18606468777',
      phoneExtension: null,
      latitude: 41.7758,
      longitude: -72.5212,
      isClosed: false
    },
    {
      name: 'Lang\'s Bowlarama',
      street: '225 Niantic Avenue',
      unit: null,
      city: 'Cranston',
      state: 'RI',
      zipCode: '02910',
      phoneNumber: '14019448880',
      phoneExtension: null,
      latitude: 41.7798,
      longitude: -71.4275,
      isClosed: false
    },
    {
      name: 'Closed Center Example',
      street: '999 Test Street',
      unit: null,
      city: 'Providence',
      state: 'RI',
      zipCode: '02903',
      phoneNumber: '14015551234',
      phoneExtension: null,
      latitude: 41.8240,
      longitude: -71.4128,
      isClosed: true
    }
  ],
  totalItems: 6
};

const MOCK_HALL_OF_FAME = {
  items: [
    { year: 2023, name: 'John Smith', category: 'Player' },
    { year: 2023, name: 'Jane Doe', category: 'Player' },
    { year: 2022, name: 'Bob Johnson', category: 'Contributor' }
  ],
  totalItems: 3
};

export function startMockApiServer(port: number = 5150): Promise<{ close: () => Promise<void> }> {
  return new Promise((resolve) => {
    const server = createServer((req: IncomingMessage, res: ServerResponse) => {
      // Enable CORS
      res.setHeader('Access-Control-Allow-Origin', '*');
      res.setHeader('Access-Control-Allow-Methods', 'GET, OPTIONS');
      res.setHeader('Access-Control-Allow-Headers', 'Content-Type');

      if (req.method === 'OPTIONS') {
        res.writeHead(200);
        res.end();
        return;
      }

      // Handle /bylaws endpoint
      if (req.url === '/bylaws' && req.method === 'GET') {
        res.writeHead(200, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify({
          content: MOCK_DOCUMENT_HTML,
          metadata: {
            LastUpdatedUtc: '2024-01-01T00:00:00Z',
            LastUpdatedBy: 'Test User'
          }
        }));
        return;
      }

      // Handle /bylaws/refresh/status endpoint
      if (req.url === '/bylaws/refresh/status' && req.method === 'GET') {
        res.writeHead(200, { 'Content-Type': 'text/event-stream' });
        res.end('data: {"status":"Completed"}\n\n');
        return;
      }

      // Handle /bowling-centers endpoint
      if (req.url === '/bowling-centers' && req.method === 'GET') {
        res.writeHead(200, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify(MOCK_BOWLING_CENTERS));
        return;
      }

      // Handle /hall-of-fame endpoint
      if (req.url === '/hall-of-fame' && req.method === 'GET') {
        res.writeHead(200, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify(MOCK_HALL_OF_FAME));
        return;
      }

      // Handle /tournaments/rules endpoint
      if (req.url === '/tournaments/rules' && req.method === 'GET') {
        res.writeHead(200, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify({
          content: '<div><h1>Tournament Rules</h1><p>Official NEBA tournament rules...</p></div>',
          metadata: {
            LastUpdatedUtc: '2024-01-01T00:00:00Z',
            LastUpdatedBy: 'Test User'
          }
        }));
        return;
      }

      // Handle /titles endpoint
      if (req.url === '/titles' && req.method === 'GET') {
        res.writeHead(200, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify({
          items: [
            { bowlerName: 'John Smith', year: 2023, tournamentName: 'Spring Classic', division: 'A' },
            { bowlerName: 'Jane Doe', year: 2023, tournamentName: 'Fall Championship', division: 'A' }
          ],
          totalItems: 2
        }));
        return;
      }

      // Handle /titles/summary endpoint
      if (req.url === '/titles/summary' && req.method === 'GET') {
        res.writeHead(200, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify({
          items: [
            { bowlerName: 'John Smith', titleCount: 5 },
            { bowlerName: 'Jane Doe', titleCount: 3 }
          ],
          totalItems: 2
        }));
        return;
      }

      // Handle /awards/bowler-of-the-year endpoint
      if (req.url === '/awards/bowler-of-the-year' && req.method === 'GET') {
        res.writeHead(200, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify({
          items: [
            { year: 2023, bowlerName: 'John Smith', division: 'Open' },
            { year: 2022, bowlerName: 'Jane Doe', division: 'Open' }
          ],
          totalItems: 2
        }));
        return;
      }

      // Handle /awards/high-block endpoint
      if (req.url === '/awards/high-block' && req.method === 'GET') {
        res.writeHead(200, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify({
          items: [
            { year: 2023, bowlerName: 'John Smith', score: 850 },
            { year: 2022, bowlerName: 'Jane Doe', score: 840 }
          ],
          totalItems: 2
        }));
        return;
      }

      // Handle /awards/high-average endpoint
      if (req.url === '/awards/high-average' && req.method === 'GET') {
        res.writeHead(200, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify({
          items: [
            { year: 2023, bowlerName: 'John Smith', average: 235.5 },
            { year: 2022, bowlerName: 'Jane Doe', average: 232.8 }
          ],
          totalItems: 2
        }));
        return;
      }

      // Default 404 response
      res.writeHead(404, { 'Content-Type': 'application/json' });
      res.end(JSON.stringify({ error: 'Not Found' }));
    });

    server.listen(port, () => {
      console.log(`Mock API server listening on http://localhost:${port}`);
      resolve({
        close: () => {
          return new Promise((resolveClose) => {
            server.close(() => {
              console.log('Mock API server closed');
              resolveClose();
            });
          });
        }
      });
    });
  });
}
