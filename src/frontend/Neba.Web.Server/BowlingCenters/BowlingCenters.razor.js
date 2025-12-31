export function scrollToTop() {
    const element = document.querySelector('#centers-scroll-container');
    if (element) {
        element.scrollTop = 0;
    }
}
