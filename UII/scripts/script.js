$(document).ready(function () {

    var timeFrame = "hour"
    var dateFormat = "{0:dd MMMM yyyy HH:mm:ss}"
    var apiUrlWithoutParameters = "https://localhost:44381/api/Charts/get-dates-per-hour?timeFrame="
    var ApiUrl = apiUrlWithoutParameters + timeFrame;

    CreateNewDataSource(ApiUrl);

    $("#KpisChart").kendoChart({
        dataSource: {
            data: [],
            sort: {
                field: "time",
                dir: "asc"
            }
        },
        title: {
            text: "MAX RX LEVEL",
            font: "20px sans-serif",
            color: "#0000FF"
        },
        seriesDefaults: {
            type: "line",
            color: "#0000FF"
        },
        series: [{
            field: "maX_RX_LEVEL",
            categoryField: "time"
        }],
        seriesClick: function (e) {
            filterGrid(e.category);
        },
        axisLabelClick: function (e) {
            filterGrid(e.value);
        },
        categoryAxis: {
            labels: {
                rotation: -45,
                visual: function (e) {
                    var visual = e.createVisual();
                    visual.options.cursor = "default";
                    return visual;
                }
            }
        },
        valueAxis: {
            title: {
                text: "RX LEVEL"
            },
            labels: {
                format: "{0:n0}"
            }
        },
        tooltip: {
            visible: true,
            template:"#= dataItem.link #"
        }
    });

    $("#KpisGrid").kendoGrid({
        dataSource: gridDataSource,
        dataBound: function (e) {
            var grid = e.sender,
                chart = $("#KpisChart").data("kendoChart");

            chart.dataSource.data(grid.dataSource.data());
        },
        height: 400,
        pageable: true,
        sortable: true,
        filterable: true,
        columns: [{
            field: "time",
            title: "Time",
            format: "{0:dd MMMM yyyy HH:mm:ss}",
            width: 200
        }, {
            field: "link",
            title: "Link",
            width: 100
        }, {
            field: "neType",
            title: "NEType"
        }, {
            field: "neAlias",
            title: "NEAlias",
        }, {
            field: "maX_RX_LEVEL",
            title: "MAX_RX_LEVEL",
        }, {
            field: "maX_TX_LEVEL",
            title: "MAX_TX_LEVEL"
        }, {
            field: "rsL_DEVIATION",
            title: "RSL_DEVIATION",
        }]
    });

    var charts = $("#KpisChart").data("kendoChart");

    $("#KPIS").change(function () {
        var item = $('#KPIS option:selected');
        charts.options.series[0].field = item.val();
        charts.options.title.text = item.text();
        charts.refresh();
    });

    var grid = $("#KpisGrid").data("kendoGrid");

    $("#FilterDates").on("click", function () {
        var dateFrom = $("#dateFrom").val();
        var dateTo = $("#dateTo").val();
        ApiUrl += "&dateFrom=" + dateFrom + "&dateTo=" + dateTo;
        url = ApiUrl
        CreateNewDataSource(url);
        grid.setDataSource(gridDataSource)
        grid.refresh();
    });

    $("#TimeFrame").on("click", function () {
        if (timeFrame === "day") {
            timeFrame = "hour";
            grid.options.columns[0].dateFormat = "{0:dd MMMM yyyy HH:mm:ss}"
            $("#TimeFrame").text("Daily")
        }
        else {
            timeFrame = "day";
            grid.options.columns[0].dateFormat = "{0:dd MMMM yyyy}"
            $("#TimeFrame").text("Hourly")
        }
        ApiUrl = apiUrlWithoutParameters + timeFrame
        CreateNewDataSource(ApiUrl);
        grid.setDataSource(gridDataSource)
        grid.refresh();
        var chart = $("#KpisChart").data("kendoChart");
        chart.refresh();
    });

    function CreateNewDataSource(url) {
        gridDataSource = new kendo.data.DataSource({
            transport: { read: { url: url, dataType: "json" } },
            schema: {
                model: {
                    fields: {
                        time: { type: "date" },
                        link: { type: "string" },
                        slot: { type: "string" },
                        neType: { type: "string" },
                        neAlias: { type: "string" },
                        maX_RX_LEVEL: { type: "number" },
                        maX_TX_LEVEL: { type: "number" },
                        rsL_DEVIATION: { type: "number" }
                    }
                }
            },
            pageSize: 10,
            sort: {
                field: "time",
                dir: "desc"
            }
        });
    }

    $("#clearGridFilter").kendoButton({
        click: function (e) {
            $("#KpisGrid").data("kendoGrid").dataSource.filter({});
        }
    });
    function filterGrid(link) {
        $("#KpisGrid").data("kendoGrid").dataSource.filter({
            field: "time",
            operator: "eq",
            value: link
        });
    }
});

