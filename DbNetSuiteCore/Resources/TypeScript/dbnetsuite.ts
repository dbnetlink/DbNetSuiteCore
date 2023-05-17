type EventName = "onRowTransform" | "onNestedClick" | "onCellTransform" | "onPageLoaded" | "onRowSelected" | "onCellDataDownload" | "onViewRecordSelected" | "onInitialized"

interface CellDataDownloadArgs {
    row: HTMLTableRowElement,
    cell: HTMLTableCellElement,
    extension: string,
    fileName: string,
    columnName: string,
    image?: HTMLImageElement
}

interface ViewRecordSelectedArgs {
    dialog: JQuery<HTMLElement>,
    record: Dictionary<String>
}

class DbNetSuite {

    public datePickerOptions: JQueryUI.DatepickerOptions = {};
    protected eventHandlers: Dictionary<Array<Function>> = {};

    constructor() {
    }

    bind(event: EventName, handler: Function) {
        if (!this.eventHandlers[event])
            this.eventHandlers[event] = [];
        this.eventHandlers[event].push(handler);
    }

    unbind(event: EventName, handler: Function) {
        if (this.eventHandlers[event] == null)
            return;

        this.eventHandlers[event].forEach((f) => {
            if (f == handler)
                f = function () { };
        })
    }

    public fireEvent(event: EventName, params: Object | undefined = undefined) {
        if (!this.eventHandlers[event])
            return false;

        var events = this.eventHandlers[event];

        events.forEach((method: Function) => {
            var args = [this];
            if (params)
                args = args.concat(Array.prototype.slice.call(arguments, 1));
            method.apply(window, args);
        })
    }
}

document.addEventListener("DOMContentLoaded", function () {
    if ($.fn.button && $.fn.button.noConflict !== undefined) {
        $.fn.bootstrapBtn = $.fn.button.noConflict();
    }
});