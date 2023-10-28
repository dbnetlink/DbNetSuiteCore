/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
declare class DbNetFile extends DbNetSuite {
    folder: string;
    folderPanel: JQuery<HTMLElement> | undefined;
    constructor(id: string);
    initialize(): void;
    reload(): void;
    private configurePage;
    callServer(action: string): void;
    private getRequest;
}
