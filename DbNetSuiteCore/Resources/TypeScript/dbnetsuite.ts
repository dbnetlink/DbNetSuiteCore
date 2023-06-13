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

type EventHandler = {
    sender: DbNetSuite;
    params: object | undefined;
};

class DbNetSuite {
    public datePickerOptions: JQueryUI.DatepickerOptions = {};
    protected element: JQuery<HTMLElement> | undefined = undefined;
    protected eventHandlers: Dictionary<Array<EventHandler>> = {};
    protected id = "";
    protected loadingPanel: JQuery<HTMLElement> | undefined;
    protected connectionString = "";
    protected connectionType: DbConnectionType = "SqlServer";
    protected initialised = false;

    bind(event: EventName, handler: EventHandler) {
        if (!this.eventHandlers[event])
            this.eventHandlers[event] = [];
        this.eventHandlers[event].push(handler);
    }

    unbind(event: EventName, handler: EventHandler) {
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
            alert("DbNetSuite stylesheet not found. Add @DbNetSuiteCore.StyleSheet() to your Razor page. See console for details.");
            console.error("DbNetSuite stylesheet not found. See https://dbnetsuitecore.z35.web.core.windows.net/index.htm?context=20#DbNetSuiteCoreStyleSheet");
        }
    }

    public fireEvent(event: EventName, params: object | undefined = undefined) {
        if (!this.eventHandlers[event])
            return false;

        const events = this.eventHandlers[event];

        events.forEach((method: EventHandler) => {
            let args:object[] = [this];

            if (params) {
                args = args.concat([params]);
            }
            method.apply(window, args);
        })
    }

    protected addPanel(panelId: string, parent: JQuery<HTMLElement> | undefined = undefined): JQuery<HTMLElement> {
        const id = `${this.id}_${panelId}Panel`;
        if (parent == null) {
            parent = this.element;
        }
        jQuery('<div>', {
            id: id
        }).appendTo(parent as JQuery<HTMLElement>);
        return $(`#${id}`);
    }

    protected addLoadingPanel() {
        this.loadingPanel = this.addPanel("loading");
        this.addPanel("loadingIcon", this.loadingPanel);
        this.loadingPanel.addClass("dbnetgrid-loading");
        this.loadingPanel.children().first().addClass("icon");
    }

    protected error(text: string) {
        const errorPanel = this.addPanel("error");
        errorPanel.addClass("dbnetsuite-error");
        errorPanel.html(text);
    }

    protected showLoader() {
        this.loadingPanel?.addClass("display");
    }

    protected hideLoader() {
        this.loadingPanel?.removeClass("display");
    }
}

document.addEventListener("DOMContentLoaded", function () {
    if ($.fn.button && $.fn.button.noConflict !== undefined) {
        $.fn.bootstrapBtn = $.fn.button.noConflict();
    }
});