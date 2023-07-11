class MessageBox extends Dialog {
    constructor(id: string) {
        super(id);
    }

    public show(message: string, type: MessageBoxType, callback: Function) {
        if (this.messageBox == undefined) {
            this.post<DbNetSuiteResponse>("message-box", {})
                .then((response) => {
                    this.element?.append(response.dialog);
                    this.messageBox = new MessageBox(`${this.id}_message_box`);
                });
        }
        this.messageBox.show(message, type)
    }
   
}