interface DbNetEditResponse extends DbNetSuiteResponse {
    toolbar: string;
    form: string;
    currentRow: number;
    totalRows: number;
    columns?: EditColumn[];
    record?: Dictionary<object>;
    searchParams?: Array<SearchParam>;
    primaryKey?: string;
}
