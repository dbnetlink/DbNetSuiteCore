/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
type DbNetFileResponseCallback = (response: DbNetFileResponse) => void;
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
    searchDialog: FileSearchDialog | undefined;
    searchFilterJoin: string;
    searchParams: Array<SearchParam>;
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
    searchResultsControl: DbNetFile | undefined;
    searchResultsDialog: SearchResultsDialog | undefined;
    searchResultsDialogId: string;
    isSearchResults: boolean;
    includeSubfolders: boolean;
    treeView: boolean;
    filesOnly: boolean;
    constructor(id: string);
    initialize(): void;
    setColumnTypes(...types: string[]): void;
    setColumnProperty(columnType: string, property: string, propertyValue: object): void;
    reload(): void;
    getPage(callback?: DbNetFileResponseCallback): void;
    private configureToolbar;
    private configurePage;
    private configureTreeView;
    private selectTreeFolder;
    configureLinkedControl(control: DbNetFile, folder: object): void;
    private openCloseFolder;
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
    callServer(action: string, callback?: DbNetFileResponseCallback): void;
    private addEventListener;
    private handleClick;
    applySearch(searchFilterJoin: string, includeSubfolders: boolean): void;
    private openSearchResultsDialog;
    private openSearchDialog;
    private getRequest;
}
declare class FileColumn {
    type?: string;
    format?: string;
    label?: string;
    constructor(type: string);
}
