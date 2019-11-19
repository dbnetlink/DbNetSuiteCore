/// <reference path="Ajax.ts" />
/// <reference path="Interfaces.ts" />

class DbNetGrid extends Ajax {
    configuration: DbNetGridConfiguration;
    $container: HTMLElement
    $toolbar: HTMLElement
    $prevBtn: HTMLButtonElement
    $nextBtn: HTMLButtonElement
    $searchBtn: HTMLButtonElement
    $searchToken: HTMLInputElement
    quickSearchTimeout: any;

    constructor(configuration: DbNetGridConfiguration) {
        super();
        this.configuration = configuration;
        this.init();
    }

    private init() {
        this.addCss('handler.dbnetsuite?handler=css');
        this.callServer("Init", this.configuration, (response) => { this.initCallback(response) });
    }

    private addCss(fileName) {
        var head = document.head;
        var link = document.createElement("link");

        link.type = "text/css";
        link.rel = "stylesheet";
        link.href = `${this.baseUrl()}${fileName}`;

        head.appendChild(link);
    }

    private baseUrl() {
        var getUrl = window.location;
        return `${getUrl.protocol}//${getUrl.host}/${getUrl.pathname.split('/')[1]}`;
    }

    private initCallback(response: DbNetGridConfiguration) {
        this.$container = document.querySelector(`#${this.configuration.id}`)
        this.$container.innerHTML = response.html.toolbar;
        this.$toolbar = this.$container.querySelector(".toolbar");
        this.$prevBtn = this.$container.querySelector("button.prev");
        this.$nextBtn = this.$container.querySelector("button.next");
        this.$searchBtn = this.$container.querySelector("button.search");
        this.$searchToken = this.$container.querySelector("input.search-token");
        this.$prevBtn.onclick = () => this.prevPage();
        this.$nextBtn.onclick = () => this.nextPage();
        this.$searchBtn.onclick = () => this.applySearch(this.$searchToken.value);
        this.quickSearchTimeout = null;
        this.$searchToken.onkeyup = (e) => this.checkSearchBox(e);
        this.$searchToken.onclick = (e) => this.checkSearchBox(e);
        this.pageCallback(response);
    } 

    private checkSearchBox(event: Event) {
        let _this = this;
        let token = <HTMLInputElement>event.target;
        clearTimeout(this.quickSearchTimeout);
        this.quickSearchTimeout = setTimeout(function () {
            if (token.value.length > 3) {
                _this.applySearch(token.value);
            }
            else if (token.getAttribute("applied-search-token"))
            {
                _this.applySearch(""); 
            }
        }, 1000);
    } 

    private nextPage() {
        if (this.configuration.currentPage >= this.configuration.totalPages) {
            return;
        }
        this.configuration.currentPage++;
        this.callServer("Page", this.configuration, (response) => { this.pageCallback(response) });
    }

    private prevPage() {
        if (this.configuration.currentPage <= 1) {
            return;
        }
        this.configuration.currentPage--;
        this.callServer("Page", this.configuration, (response) => { this.pageCallback(response) });
    }

    private applySearch(token: string) {
        this.$searchToken.setAttribute("applied-search-token", token);
        this.configuration.searchToken = token.toString();
        this.configuration.currentPage = 1;
        this.callServer("Page", this.configuration, (response) => { this.pageCallback(response) });
    }

    private pageCallback(response: DbNetGridConfiguration) {
        this.configuration = response;
        this.$container.querySelector(".grid").innerHTML = response.html.page;
        (<HTMLElement>this.$container.querySelector(".current-page")).innerText = response.currentPage.toString();
        (<HTMLElement>this.$container.querySelector(".total-pages")).innerText = response.totalPages.toString();
        this.$prevBtn.disabled = (response.currentPage === 1);
        this.$nextBtn.disabled = (response.currentPage === response.totalPages);
    }
}