interface DbNetComboRequest
{
    addEmptyOption: boolean;
    addFilter: boolean;
    componentId: string;
    connectionString: string;
    connectionType?: string;
    emptyOptionText: string;
    filterToken: string;
    foreignKeyColumn : string;
    foreignKeyValue?: Array<string> | undefined;
    fromPart: string;
    multipleSelect: boolean;
    params: Dictionary<object>;
    size: number;
    textColumn: string;
    valueColumn: string;
}