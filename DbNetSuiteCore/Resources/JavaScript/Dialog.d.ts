/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
declare class Dialog extends DbNetSuite {
    $dialog: JQuery<HTMLElement> | undefined;
    windowWidth: number;
    windowHeight: number;
    constructor(id: string);
    open(): void;
    isOpen(): boolean;
    close(): void;
    message(msg: string): void;
    clearMessage(): void;
    protected button(type: string): JQuery<HTMLElement>;
}
