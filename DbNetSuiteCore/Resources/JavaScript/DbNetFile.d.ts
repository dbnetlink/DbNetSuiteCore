/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
declare class DbNetFile extends DbNetSuite {
    rootFolder: string;
    folder: string;
    folderPanel: JQuery<HTMLElement> | undefined;
    toolbarPanel: JQuery<HTMLElement> | undefined;
    columns: FileColumn[];
    quickSearch: boolean;
    quickSearchToken: string;
    toolbarButtonStyle: ToolbarButtonStyle;
    search: boolean;
    export: boolean;
    copy: boolean;
    upload: boolean;
    navigation: boolean;
    totalRows: number;
    totalPages: number;
    currentPage: number;
    pageSize: number;
    caption: string;
    nested: boolean;
    constructor(id: string);
    initialize(): void;
    setColumnTypes(...types: string[]): void;
    reload(): void;
    private configureToolbar;
    private configurePage;
    private selectFolder;
    private openNestedFolder;
    callServer(action: string): void;
    private addEventListener;
    private handleClick;
    private getRequest;
}
