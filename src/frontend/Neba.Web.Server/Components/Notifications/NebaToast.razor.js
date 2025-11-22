// Create a timer instance for each toast
export function createTimer(componentRef, duration) {
    const timer = {
        timerId: null,
        dotNetRef: componentRef,
        remainingTime: duration,
        startTime: Date.now(),
        isPaused: false,

        start() {
            this.timerId = setTimeout(async () => {
                if (this.dotNetRef) {
                    await this.dotNetRef.invokeMethodAsync('OnTimerExpired');
                }
            }, this.remainingTime);
        },

        pause() {
            if (!this.isPaused && this.timerId) {
                clearTimeout(this.timerId);
                this.remainingTime -= (Date.now() - this.startTime);
                this.isPaused = true;
            }
        },

        resume() {
            if (this.isPaused && this.dotNetRef) {
                this.startTime = Date.now();
                this.isPaused = false;

                this.timerId = setTimeout(async () => {
                    if (this.dotNetRef) {
                        await this.dotNetRef.invokeMethodAsync('OnTimerExpired');
                    }
                }, this.remainingTime);
            }
        },

        cancel() {
            if (this.timerId) {
                clearTimeout(this.timerId);
                this.timerId = null;
            }
            this.dotNetRef = null;
            this.remainingTime = 0;
            this.isPaused = false;
        }
    };

    timer.start();
    return timer;
}
