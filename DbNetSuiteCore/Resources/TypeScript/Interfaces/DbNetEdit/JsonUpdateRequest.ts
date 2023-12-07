interface JsonUpdateRequest {
    primaryKey: string;
    editMode: string;
    changes: Dictionary<object>;
    formData: FormData;
}