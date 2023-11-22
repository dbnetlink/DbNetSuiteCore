"use strict";
class SearchResultsDialog extends Dialog {
    constructor(id, parentControl) {
        super(id);
        this.parentControl = parentControl;
    }
    show() {
        this.open();
    }
}
