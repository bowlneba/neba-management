let timerId = null;
let dotNetRef = null;
let remainingTime = 0;
let startTime = 0;
let isPaused = false;

export function startTimer(componentRef, duration) {
    dotNetRef = componentRef;
    remainingTime = duration;
    startTime = Date.now();
    isPaused = false;

    timerId = setTimeout(async () => {
        if (dotNetRef) {
            await dotNetRef.invokeMethodAsync('OnTimerExpired');
        }
    }, duration);
}

export function pauseTimer() {
    if (!isPaused && timerId) {
        clearTimeout(timerId);
        remainingTime -= (Date.now() - startTime);
        isPaused = true;
    }
}

export function resumeTimer() {
    if (isPaused && dotNetRef) {
        startTime = Date.now();
        isPaused = false;

        timerId = setTimeout(async () => {
            if (dotNetRef) {
                await dotNetRef.invokeMethodAsync('OnTimerExpired');
            }
        }, remainingTime);
    }
}

export function cancelTimer() {
    if (timerId) {
        clearTimeout(timerId);
        timerId = null;
    }
    dotNetRef = null;
    remainingTime = 0;
    isPaused = false;
}
