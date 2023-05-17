/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
declare class Dialog {
    $dialog: JQuery<HTMLElement> | undefined;
    width: number;
    maxWidth: number;
    constructor(id: string);
    open(): void;
    isOpen(): boolean;
    close(): void;
    message(msg: string): void;
    clearMessage(): void;
}
