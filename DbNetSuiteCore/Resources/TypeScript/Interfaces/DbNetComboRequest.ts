interface DbNetComboRequest
{
    componentId: string;
    connectionString: string;
    connectionType?: string;
    sql: string | undefined;
    params: Dictionary<object>;
    addEmptyOption: boolean;
    emptyOptionText: string;
}