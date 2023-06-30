interface DbNetEditRequest extends DbNetSuiteRequest { 
    columns: EditColumnRequest[];
    currentRow: number | undefined;
    fromPart: string;
    search:boolean;
    navigation: boolean;
    quickSearch: boolean;
}