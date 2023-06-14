interface DbNetComboRequest
{
    addEmptyOption: boolean;
    addFilter: boolean;
    componentId: string;
    connectionString: string;
    connectionType?: string;
    emptyOptionText: string;
    params: Dictionary<object>;
    fromPart: string;
    valueColumn: string;
    textColumn: string;
    filterToken: string;
    foreignKeyColumn : string;
    foreignKeyValue: object;
}