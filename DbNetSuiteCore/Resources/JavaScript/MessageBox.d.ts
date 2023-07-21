/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
declare enum MessageBoxType {
    Error = 0,
    Confirm = 1,
    Info = 2
}
declare enum MessageBoxButtonType {
    Confirm = 0,
    Cancel = 1,
    Ok = 2
}
type MessageBoxCallback = (buttonPressed: MessageBoxButtonType) => void;
declare class MessageBox extends Dialog {
    callback: MessageBoxCallback | undefined;
    $text: JQuery<HTMLElement>;
    $icon: JQuery<HTMLElement>;
    constructor(id: string);
    show(messageBoxType: MessageBoxType, text: string, element: JQuery<HTMLElement> | null | undefined, callback: MessageBoxCallback | undefined): void;
    private hideButtons;
    private button;
    private buttonPressed;
}
