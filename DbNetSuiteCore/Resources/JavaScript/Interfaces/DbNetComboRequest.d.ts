interface DbNetComboRequest {
    addEmptyOption: boolean;
    addFilter: boolean;
    componentId: string;
    connectionString: string;
    connectionType?: string;
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
