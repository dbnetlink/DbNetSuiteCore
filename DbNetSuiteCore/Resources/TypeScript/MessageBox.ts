class MessageBox extends Dialog {
    constructor(id: string) {
        super(id);
    }

    public show(text: string) {
        this.$dialog?.find(".message-box-text").html(text)
        this.open();
    }
}