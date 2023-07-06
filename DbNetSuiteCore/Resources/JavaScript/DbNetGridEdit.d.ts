/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
declare class DbNetGridEdit extends DbNetSuite {
    columns: DbColumn[] | undefined;
    fromPart: string;
    lookupDialog: LookupDialog | undefined;
    navigation: boolean;
    quickSearch: boolean;
    quickSearchDelay: number;
    quickSearchMinChars: number;
    quickSearchTimerId: number | undefined;
    quickSearchToken: string;
    search: boolean;
    searchDialog: SearchDialog | undefined;
    searchFilterJoin: string;
    searchParams: Array<SearchParam>;
    toolbarButtonStyle: ToolbarButtonStyle;
    toolbarPanel: JQuery<HTMLElement> | undefined;
    toolbarPosition: ToolbarPosition;
    constructor(id: string);
}
