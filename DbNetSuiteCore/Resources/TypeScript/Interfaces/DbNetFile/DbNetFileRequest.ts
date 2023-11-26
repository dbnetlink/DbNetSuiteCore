interface DbNetFileRequest extends DbNetSuiteRequest
{
    rootFolder: string;
    folder: string;
    columns: FileColumnRequest[];
    quickSearch: boolean;
    quickSearchToken: string;
    toolbarButtonStyle: ToolbarButtonStyle;
    search: boolean;
    searchFilterJoin: string;
    searchParams: Array<SearchParam>;
    export: boolean;
    copy: boolean;
    upload: boolean;
    navigation: boolean;
    pageSize: number;
    currentPage: number;
    caption: string;
    nested: boolean;
    fileName: string;
    orderBy: string;
    orderByDirection: string;
    isSearchResults: boolean;
    includeSubfolders: boolean;
    treeView: boolean;
    filesOnly: boolean;
}