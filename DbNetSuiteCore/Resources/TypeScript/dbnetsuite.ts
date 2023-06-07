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
    record: Dictionary<string>
}

class DbNetSuite {

    public datePickerOptions: JQueryUI.DatepickerOptions = {};
    protected eventHandlers: Dictionary<Array<Function>> = {};

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

    checkStyleSheetLoaded() {
        let found = false;
        for (const sheet of document.styleSheets) {
            if (sheet.href) {
                if (sheet?.href?.indexOf("resource.dbnetsuite?action=css") > 0) {
                    found = true;
                }
            }
        }

        if (!found) {
            alert("DbNetSuite stylesheet not found. Add @DbNetSuiteCore.StyleSheet() to yoiur Razor page. See console for details.");
            console.error("DbNetSuite stylesheet not found. See https://dbnetsuitecore.z35.web.core.windows.net/index.htm?context=20#DbNetSuiteCoreStyleSheet");
        }
    }

    public fireEvent(event: EventName, params: object | undefined = undefined) {
        if (!this.eventHandlers[event])
            return false;

        const events = this.eventHandlers[event];

        events.forEach((method: Function) => {
            let args:object[] = [this];

            if (params) {
                args = args.concat([params]);
            }
            method.apply(window, args);
        })
    }
}

document.addEventListener("DOMContentLoaded", function () {
    if ($.fn.button && $.fn.button.noConflict !== undefined) {
        $.fn.bootstrapBtn = $.fn.button.noConflict();
    }
});