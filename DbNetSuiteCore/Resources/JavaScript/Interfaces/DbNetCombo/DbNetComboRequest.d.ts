interface DbNetComboRequest extends DbNetSuiteRequest {
    addEmptyOption: boolean;
    addFilter: boolean;
    dataOnlyColumns: Array<string> | undefined;
    distinct: boolean;
    emptyOptionText: string;
    filterToken: string;
    foreignKeyColumn: string;
    foreignKeyValue?: Array<string> | undefined;
    fromPart: string;
    multipleSelect: boolean;
    procedureParams: Dictionary<object>;
    procedureName: string | undefined;
    size: number;
    textColumn: string;
    valueColumn: string;
}
