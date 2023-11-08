/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
declare class DbNetFile extends DbNetSuite {
    rootFolder: string;
    folder: string;
    folderPanel: JQuery<HTMLElement> | undefined;
    columns: FileColumn[];
    constructor(id: string);
    initialize(): void;
    setColumnTypes(...types: string[]): void;
    reload(): void;
    private configurePage;
    private selectFolder;
    callServer(action: string): void;
    private getRequest;
}
