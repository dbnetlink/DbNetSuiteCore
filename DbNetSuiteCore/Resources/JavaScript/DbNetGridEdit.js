"use strict";
class DbNetGridEdit extends DbNetSuite {
    constructor(id) {
        super(id);
        this.fromPart = "";
        this.navigation = true;
        this.quickSearch = false;
        this.quickSearchDelay = 1000;
        this.quickSearchMinChars = 3;
        this.quickSearchToken = "";
        this.search = true;
        this.searchFilterJoin = "";
        this.searchParams = [];
        this.toolbarButtonStyle = ToolbarButtonStyle.Image;
        this.toolbarPosition = "Top";
    }
}
