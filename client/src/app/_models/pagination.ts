export interface Pagination {

    currentPage : number;
    itemsPerPage :number;
    totalItems : number;
    totalPages  : number;
}

export class PaginationedResult<T> {
    items?:T;
    pagination? : Pagination
}