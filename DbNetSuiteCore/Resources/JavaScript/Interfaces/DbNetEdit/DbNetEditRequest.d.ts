interface DbNetEditRequest extends DbNetGridEditRequest {
    changes: Dictionary<object>;
    columns: EditColumnRequest[];
    currentRow: number | undefined;
    totalRows: number;
    primaryKey: string;
    layoutColumns: number;
    isEditDialog: boolean;
    toolbarPosition: ToolbarPosition;
}
