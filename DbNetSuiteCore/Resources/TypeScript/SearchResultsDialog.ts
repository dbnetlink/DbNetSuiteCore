class SearchResultsDialog extends Dialog {
    parentControl: DbNetFile;
    constructor(id: string, parentControl: DbNetFile) {
        super(id);
        this.parentControl = parentControl;
    }

    show(): void {
        this.open();
    }
}