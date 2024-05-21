interface JsonUpdateRequest {
    primaryKey: string;
    editMode: EditMode;
    changes?: Dictionary<object>;
    formData?: FormData;
    columns?: DbColumn[];
}
