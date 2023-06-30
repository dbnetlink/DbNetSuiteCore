/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
declare class DbNetEdit extends DbNetSuite {
    columns: EditColumn[];
    currentRow: number;
    formPanel: JQuery<HTMLElement> | undefined;
    toolbarPanel: JQuery<HTMLElement> | undefined;
    toolbarPosition: ToolbarPosition;
    linkedControls: Array<DbNetSuite>;
    fromPart: string;
    search: boolean;
    navigation: boolean;
    quickSearch: boolean;
    constructor(id: string);
    initialize(): void;
    addLinkedControl(control: DbNetSuite): void;
    reload(): void;
    private configureEdit;
    callServer(action: string): void;
    private getRequest;
}
