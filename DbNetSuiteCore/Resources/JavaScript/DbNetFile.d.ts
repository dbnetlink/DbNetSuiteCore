/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
declare class DbNetFile extends DbNetSuite {
    rootFolder: string;
    fileName: string;
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
    previewHeight: number;
    caption: string;
    nested: boolean;
    orderBy: string;
    orderByDirection: string;
    constructor(id: string);
    initialize(): void;
    setColumnTypes(...types: string[]): void;
    setColumnProperty(columnType: string, property: string, propertyValue: object): void;
    reload(): void;
    getPage(): void;
    private configureToolbar;
    private configurePage;
    private selectFolder;
    private loadPreview;
    private setPreviewHeight;
    private selectFile;
    private addRowEventHandlers;
    private handleRowClick;
    private handleHeaderClick;
    private downloadBlob;
    private viewInTab;
    private openNestedFolder;
    callServer(action: string): void;
    private addEventListener;
    private handleClick;
    private getRequest;
}
declare class FileColumn {
    type?: string;
    format?: string;
    label?: string;
    constructor(type: string);
}
