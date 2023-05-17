"use strict";
class DbNetSuite {
    constructor() {
        this.datePickerOptions = {};
        this.eventHandlers = {};
    }
    bind(event, handler) {
        if (!this.eventHandlers[event])
            this.eventHandlers[event] = [];
        this.eventHandlers[event].push(handler);
    }
    unbind(event, handler) {
        if (this.eventHandlers[event] == null)
            return;
        this.eventHandlers[event].forEach((f) => {
            if (f == handler)
                f = function () { };
        });
    }
    fireEvent(event, params = undefined) {
        if (!this.eventHandlers[event])
            return false;
        var events = this.eventHandlers[event];
        events.forEach((method) => {
            var args = [this];
            if (params)
                args = args.concat(Array.prototype.slice.call(arguments, 1));
            method.apply(window, args);
        });
    }
}
document.addEventListener("DOMContentLoaded", function () {
    if ($.fn.button && $.fn.button.noConflict !== undefined) {
        $.fn.bootstrapBtn = $.fn.button.noConflict();
    }
});
