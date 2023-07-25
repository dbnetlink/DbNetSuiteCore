enum MessageBoxType {
    Error,
    Confirm,
    Info
}
enum MessageBoxButtonType {
    Confirm,
    Cancel,
    Ok
}

type MessageBoxCallback = (buttonPressed: MessageBoxButtonType) => void;
class MessageBox extends Dialog {

    callback: MessageBoxCallback | undefined;
    $text: JQuery<HTMLElement>;
    $icon: JQuery<HTMLElement>;
    constructor(id: string) {
        super(id);
        this.button("ok").on("click", () => this.buttonPressed(MessageBoxButtonType.Ok));
        this.button("confirm").on("click", () => this.buttonPressed(MessageBoxButtonType.Confirm));
        this.button("cancel").on("click", () => this.buttonPressed(MessageBoxButtonType.Cancel));
        this.$text = this.$dialog?.find(".message-box-text") as JQuery<HTMLElement>;
        this.$icon = this.$dialog?.find(".message-box-icon") as JQuery<HTMLElement>;
    }

    public show(messageBoxType: MessageBoxType, text: string, element: JQuery<HTMLElement> | null = null, callback: MessageBoxCallback | undefined) {

        this.$dialog?.dialog("option", "width", "auto");

        if (text.length > 200) {
            this.$dialog?.dialog("option", "width", 600);
        }

        this.$text.html(text)
        if (element) {
            this.$dialog?.dialog("option", "position", { of: element.get(0) });
        }

        this.callback = callback;

        this.hideButtons()
        this.$icon.removeClass("info").removeClass("confirm").removeClass("error")

        switch (messageBoxType) {
            case MessageBoxType.Info:
            case MessageBoxType.Error:
                this.$dialog?.dialog("option", "title", messageBoxType == MessageBoxType.Info ? "Information" : "Error");
                this.button("ok").show();
                this.$icon.addClass(messageBoxType == MessageBoxType.Info ? "info" : "error");
                break;
            case MessageBoxType.Confirm:
                this.$dialog?.dialog("option", "title", "Confirm");
                this.button("confirm").show();
                this.button("cancel").show();
                this.$icon.addClass("confirm");
                break;
        }
        this.open();
    }

    private hideButtons() {
        this.button("confirm").hide();
        this.button("cancel").hide();
        this.button("ok").hide();
    }

    private button(type: string): JQuery<HTMLElement> {
        return this.$dialog?.find(`.${type}-btn`) as JQuery<HTMLElement>;
    }

    private buttonPressed(type: MessageBoxButtonType) {
        this.close();
        if (this.callback) {
            this.callback(type);
        }
    }
}