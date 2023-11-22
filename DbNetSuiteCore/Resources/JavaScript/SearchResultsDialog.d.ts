declare class SearchResultsDialog extends Dialog {
    parentControl: DbNetFile;
    constructor(id: string, parentControl: DbNetFile);
    show(): void;
}
