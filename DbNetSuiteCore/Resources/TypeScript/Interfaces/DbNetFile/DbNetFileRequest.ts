interface DbNetFileRequest extends DbNetSuiteRequest
{
    rootFolder: string;
    folder: string;
    columns: FileColumnRequest[];
    quickSearch: boolean;
    quickSearchToken: string;
    toolbarButtonStyle: ToolbarButtonStyle;
    search: boolean;
    export: boolean;
    copy: boolean;
    upload: boolean;
    navigation: boolean;
    pageSize: number;
    currentPage: number;
    caption: string;
    nested: boolean;
}