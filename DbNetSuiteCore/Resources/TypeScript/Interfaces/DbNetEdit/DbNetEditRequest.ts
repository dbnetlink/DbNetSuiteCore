interface DbNetEditRequest extends DbNetGridEditRequest {
    changes: Dictionary<object>;
    columns: EditColumnRequest[];
    currentRow: number | undefined;
    totalRows: number;
    layoutColumns: number;
    isEditDialog: boolean;
    toolbarPosition: ToolbarPosition;
    formCacheKey: string;
}