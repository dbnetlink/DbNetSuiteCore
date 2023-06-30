interface DbNetEditResponse extends DbNetSuiteResponse {
    toolbar: string;
    form: string;
    currentRow: number;
    totalRows: number;
    columns?: EditColumn[];
    record?: Dictionary<string>;
    searchParams?: Array<SearchParam>;
}
