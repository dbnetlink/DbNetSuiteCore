interface DbNetComboRequest
{
    addEmptyOption: boolean;
    addFilter: boolean;
    componentId: string;
    connectionString: string;
    connectionType?: string;
    emptyOptionText: string;
    params: Dictionary<object>;
    sql: string;
    filterToken: string;
}