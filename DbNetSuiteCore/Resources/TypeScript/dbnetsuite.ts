type EventName = "onRowTransform" | "onNestedClick" | "onCellTransform" | "onPageLoaded" | "onRowSelected" | "onCellDataDownload" | "onViewRecordSelected" | "onInitialized" | "onOptionSelected" | "onOptionsLoaded" | "onFormElementCreated" | "onRecordUpdated" | "onRecordInserted" | "onRecordDeleted" | "onInsertInitalize" | "onFormUpdated"

interface CellDataDownloadArgs {
    row: HTMLTableRowElement,
    cell: HTMLTableCellElement,
    extension: string,
    fileName: string,
    columnName: string,
    image?: HTMLImageElement
}

interface ViewRecordSelectedArgs {
    dialog: JQuery<HTMLElement> | undefined,
    record: Dictionary<object> | undefined
}

type EventHandler = {
    sender: DbNetSuite;
    params: object | undefined;
};

type InternalEventHandler = {
    context: DbNetSuite;
    method: EventHandler;
};

type EmptyCallback = (sender: DbNetSuite, args?: object) => void;
class DbNetSuite {
    public static DBNull = "DBNull";
    public datePickerOptions: JQueryUI.DatepickerOptions = {};
    protected element: JQuery<HTMLElement> | undefined = undefined;
    protected eventHandlers: Dictionary<Array<EventHandler>> = {};
    protected internalEventHandlers: Dictionary<Array<InternalEventHandler>> = {};
    protected id = "";
    protected loadingPanel: JQuery<HTMLElement> | undefined;
    protected connectionString = "";
    protected connectionType: DbConnectionType = "SqlServer";
    protected culture = "";
    protected linkedControls: Array<DbNetSuite> = [];
    protected messageBox: MessageBox | undefined;
    protected parentControlType = "";

    public initialised = false;

    constructor(id: string | null) {
        if (id == null) {
            return;
        }
        this.id = id;
        this.element = $(`#${this.id}`) as JQuery<HTMLElement>;
        this.element.addClass("dbnetsuite").addClass("cleanslate").addClass("empty");

        this.checkStyleSheetLoaded();

        if (this.element.length == 0) {
            this.error(`${this.constructor.name} container element '${this.id}' not found`);
            return;
        }
    }

    bind(event: EventName, handler: EventHandler ) {
        if (!this.eventHandlers[event])
            this.eventHandlers[event] = [];
        this.eventHandlers[event].push(handler);
    }

    internalBind(event: EventName, callback: EmptyCallback) {
        if (!this.internalEventHandlers[event])
            this.internalEventHandlers[event] = [];
        this.internalEventHandlers[event].push({ sender: this, method: callback } as unknown as InternalEventHandler);
    }

    unbind(event: EventName, handler: EventHandler) {
        if (this.eventHandlers[event] == null)
            return;

        this.eventHandlers[event].forEach((f) => {
            if (f == handler)
                f = function () { return; };
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

    addLinkedControl(control: DbNetSuite) {
        //control.parentControlType = this.constructor.name;
        this.linkedControls.push(control);
    }

    fireEvent(event: EventName, params: object | undefined = undefined) {
        if (this.eventHandlers[event]) {
            const events = this.eventHandlers[event];

            events.forEach((handler: EventHandler) => {
                let args: object[] = [this];

                if (params) {
                    args = args.concat([params]);
                }
                handler.apply(window, args);
            })
        }

        if (this.internalEventHandlers[event]) {
            const events = this.internalEventHandlers[event];

            events.forEach((handler: InternalEventHandler) => {
                let args: object[] = [this];

                if (params) {
                    args = args.concat([params]);
                }
                handler.method.apply(handler.context, args);
            })
        }
    }

    protected addPanel(panelId: string, parent: JQuery<HTMLElement> | undefined = undefined): JQuery<HTMLElement> {
        const id = `${this.id}_${panelId}Panel`;
        if (parent == null) {
            parent = this.element;
        }
        jQuery('<div>', {
            id: id
        }).addClass(`${this.constructor.name.toLowerCase()}-${panelId}`).appendTo(parent as JQuery<HTMLElement>);
        return $(`#${id}`);
    }

    protected addLoadingPanel() {
        this.loadingPanel = this.addPanel("loading");
        this.addPanel("loadingIcon", this.loadingPanel);
        this.loadingPanel.addClass("dbnetsuite-loading");
        this.loadingPanel.children().first().addClass("icon");
    }

    protected showLoader() {
        this.loadingPanel?.addClass("display");
    }

    protected hideLoader() {
        this.element?.removeClass("empty")
        this.loadingPanel?.removeClass("display");
    }

    protected post<T>(action: string, request: any, blob = false, page:string|null = null): Promise<T> {
        this.showLoader();
        const options = {
            method: "POST",
            headers: {
                Accept: "application/json",
                "Content-Type": "application/json;charset=UTF-8",
            },
            body: JSON.stringify(request)
        };

        if (page == null) {
            page = this.constructor.name.toLowerCase()
        }

        return fetch(`~/${page}.dbnetsuite?action=${action}`, options)
            .then(response => {
                this.hideLoader();

                if (!response.ok) {
                    throw response;
                }
                if (blob) {
                    return response.blob() as Promise<T>;
                }
                return response.json() as Promise<T>;
            })
            .catch(err => {
                err.text().then((errorMessage: string) => {
                    console.error(errorMessage);
                    this.error(errorMessage.split("\n").shift() as string)
                });

                return Promise.reject()
            })
    }

    public controlElement(name: string): JQuery<HTMLElement> {
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        return $(`#${this.controlElementId(name)}`)!;
    }
    protected controlElementId(name: string): string {
        return `${this.id}_${name}`;
    }
    protected disable(id: string, disabled: boolean) {
        this.controlElement(id).prop("disabled", disabled);
    }
    protected setInputElement(name: string, value: number) {
        const el = this.controlElement(name);
        el.val(value.toString());
        el.width(`${value.toString().length}em`);
    }

    protected configureLinkedControls(id: object | null, pk:string | null = null) {
        this.linkedControls.forEach((control) => {
            this._configureLinkedControl(control, id, pk);
        });
    }

    protected info(text:string, element:JQuery<HTMLElement>) {
        this.showMessageBox(MessageBoxType.Info, text, element);
    }

    protected confirm(text: string, element: JQuery<HTMLElement>, callback: MessageBoxCallback) {
        this.showMessageBox(MessageBoxType.Confirm, text, element, callback);
    }

    protected error(text: string, element: JQuery<HTMLElement> | null = null) {
        this.showMessageBox(MessageBoxType.Error, text, element);
    }

    private showMessageBox(messageBoxType: MessageBoxType, text: string, element: JQuery<HTMLElement> | null = null, callback: MessageBoxCallback | undefined = undefined) {
        if (this.messageBox == undefined) {
            this.post<DbNetSuiteResponse>("message-box", this._getRequest(), false, "dbnetsuite")
                .then((response) => {
                    this.element?.append(response.html);
                    this.messageBox = new MessageBox(`${this.id}_message_box`);
                    this.messageBox?.show(messageBoxType, text, element, callback);
                });
        }
        this.messageBox?.show(messageBoxType, text, element, callback);
    }

    protected addDatePicker($input: JQuery<HTMLInputElement>, datePickerOptions: JQueryUI.DatepickerOptions) {
        const options = datePickerOptions;
        const formats = { D: "DD, MM dd, yy", DDDD: "DD", DDD: "D", MMMM: "MM", MMM: "M", M: "m", MM: "mm", yyyy: "yy" };

        let format: string = $input.attr("format") as string;

        let pattern: keyof typeof formats;
        for (pattern in formats) {
            const re = new RegExp(`\\b${pattern}\\b`);
            format = format.replace(re, formats[pattern]);
        }
        if (format != undefined)
            if (format != $input.attr("format"))
                options.dateFormat = format;

        options.onSelect = this.pickerSelected;

        $input.datepicker(options);
    }

    private pickerSelected() {
        const $row = $(this).closest("tr").parent().closest("tr");
        const $select = $row.find("select");
        if ($select.val() == "") {
            $select.prop("selectedIndex", 1)
        }
    }

    protected addTimePicker($input: JQuery<HTMLInputElement>) {
        const options = { "zindex": 100000 };
        options.change = this.pickerSelected;
        $input.timepicker(options);
    }

    private _configureLinkedControl(control: DbNetSuite, id: object | null, pk: string | null) {
        if (this instanceof DbNetGrid) {
            (this as DbNetGrid).configureLinkedControl(control, id, pk);
        }
        if (this instanceof DbNetCombo) {
            (this as DbNetCombo).configureLinkedControl(control, id as string[], pk);
        }
        if (this instanceof DbNetEdit) {
            (this as DbNetEdit).configureLinkedControl(control, pk);
        }
    }

    private _getRequest(): DbNetSuiteRequest {
        const request: DbNetSuiteRequest = {
            componentId: this.id,
            connectionString: this.connectionString,
            culture: this.culture,
            parentControlType: this.parentControlType
        };

        return request;
    }
}

document.addEventListener("DOMContentLoaded", function () {
    if ($.fn.button && $.fn.button.noConflict !== undefined) {
        $.fn.bootstrapBtn = $.fn.button.noConflict();
    }
});