interface DbNetEditResponse extends DbNetSuiteResponse {
    toolbar: string;
    form: string;
    data: string;
    currentPage: number;
    totalPages: number;
    totalRows: number;
    columns?: GridColumn[];
    record?: Dictionary<string>;
    searchParams?: Array<SearchParam>;
}

