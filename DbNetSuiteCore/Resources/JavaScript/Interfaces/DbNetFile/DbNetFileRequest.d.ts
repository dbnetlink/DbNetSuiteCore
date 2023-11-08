interface DbNetFileRequest extends DbNetSuiteRequest {
    rootFolder: string;
    folder: string;
    columns: FileColumnRequest[];
}
