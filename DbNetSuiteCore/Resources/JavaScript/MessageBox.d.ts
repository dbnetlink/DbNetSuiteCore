declare class MessageBox extends Dialog {
    constructor(id: string);
    show(message: string, type: MessageBoxType, callback: Function): void;
}
