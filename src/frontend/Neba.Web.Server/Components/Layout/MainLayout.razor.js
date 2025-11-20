// MainLayout navigation functionality

export function toggleMobileMenu() {
    const menu = document.querySelector('#main-menu');
    menu?.classList.toggle('active');
}

export function toggleDropdown(element) {
    element.closest('.neba-nav-item')?.classList.toggle('active');
}

// Initialize event listeners when DOM is ready
function initializeNavigation() {
    // Mobile menu toggle
    const menuToggle = document.querySelector('[data-action="toggle-menu"]');
    menuToggle?.addEventListener('click', toggleMobileMenu);

    // Dropdown toggle
    const dropdownToggles = document.querySelectorAll('[data-action="toggle-dropdown"]');
    dropdownToggles.forEach(toggle => {
        toggle.addEventListener('click', (event) => {
            // Prevent navigation when clicking the History link in mobile/tablet view
            if (window.innerWidth <= 1024) {
                event.preventDefault();
            }
            toggleDropdown(event.currentTarget);
        });
    });

    // Close mobile menu when clicking outside
    document.addEventListener('click', (event) => {
        const menu = document.querySelector('#main-menu');
        const menuToggle = document.querySelector('[data-action="toggle-menu"]');

        if (menu?.classList.contains('active')) {
            if (!menu.contains(event.target) && !menuToggle?.contains(event.target)) {
                menu.classList.remove('active');
            }
        }
    });

    // Close dropdowns when clicking outside
    document.addEventListener('click', (event) => {
        if (!event.target.closest('.neba-nav-item')) {
            document.querySelectorAll('.neba-nav-item.active').forEach(item => {
                item.classList.remove('active');
            });
        }
    });

    // Add scroll shadow effect to navbar
    const navbar = document.querySelector('.neba-navbar');
    let lastScrollY = window.scrollY;

    function handleScroll() {
        const currentScrollY = window.scrollY;

        if (currentScrollY > 10) {
            navbar?.classList.add('scrolled');
        } else {
            navbar?.classList.remove('scrolled');
        }

        lastScrollY = currentScrollY;
    }

    window.addEventListener('scroll', handleScroll, { passive: true });
    handleScroll(); // Initial check
}

// Initialize on DOM ready and after Blazor updates
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeNavigation);
} else {
    initializeNavigation();
}

// Re-initialize after Blazor enhanced navigation
document.addEventListener('enhancedload', initializeNavigation);
