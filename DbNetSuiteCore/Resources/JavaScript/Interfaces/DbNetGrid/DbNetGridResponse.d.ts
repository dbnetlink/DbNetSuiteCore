interface DbNetGridResponse extends DbNetGridEditResponse {
    data: string;
    currentPage: number;
    totalPages: number;
    columns?: GridColumn[];
}
