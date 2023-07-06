
class DbNetGridEdit extends DbNetSuite {
    columns: DbColumn[] | undefined;
    fromPart = "";
    lookupDialog: LookupDialog | undefined;
    navigation = true;
    quickSearch = false;
    quickSearchDelay = 1000;
    quickSearchMinChars = 3;
    quickSearchTimerId: number | undefined;
    quickSearchToken = "";
    search = true;
    searchDialog: SearchDialog | undefined;
    searchFilterJoin = "";
    searchParams: Array<SearchParam> = [];
    toolbarButtonStyle: ToolbarButtonStyle = ToolbarButtonStyle.Image;
    toolbarPanel: JQuery<HTMLElement> | undefined;
    toolbarPosition: ToolbarPosition = "Top";

    constructor(id: string) {
        super(id);
    }
}