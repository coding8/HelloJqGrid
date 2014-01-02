$(function () {
    //配置jqGrid
    $("#grid").jqGrid({
        url: "/Home/GetSearchData",
        datatype: "json",
        mtype: "get",
        colModel: [
            { label: 'Number', name: 'No', index: 'No', width: 60, sorttype: "int" },
            { label: 'Name', name: 'Name', index: 'Name', width: 90 },
            { label: 'Email', name: 'Email', index: 'Email', width: 100, sortable: false },
            {
                label: 'Birthday', name: 'Birthday', index: 'Birthday', width: 100,
                formatter: 'date', formatoptions: { srcformat: 'Y-m-d', newformat: 'Y-m-d' },
                editable: true, editrules: { required: true }
            },
            {
                label: 'Created On', name: 'CreatedOn', index: 'CreatedOn', width: 150,
                formatter: 'date', formatoptions: { srcformat: 'Y-m-d H:i:s', newformat: 'Y-m-d H:i:s' }
            }
        ],
        caption: "jqGrid Single Search",
        loadonce: false,    //排序翻页等操作在服务端完成
        rowNum: 5,
        rowList: [5, 10, 20],  //设置分页下拉列表
        pager: "#pager",
        viewrecords: true,
        width: 500, height: "auto",
        jsonReader: { repeatitems: false },
        sortorder: "asc",
        sortname: "No"//传递给服务端的排序字段名
    });
});

function clearSearchString() {
    $("#searchString").val("");
}

function singleSearch () {
    var sdata = {   //构建查询需要的参数  
        searchField: $("#searchField").val(),
        searchString: $("#searchString").val(),
        searchOper: $("#searchOper").val()
    };
    // 获得当前postData选项的值  
    var postData = $("#grid").jqGrid("getGridParam", "postData");

    //将查询参数融入postData选项对象  
    $.extend(postData, sdata);

    $("#grid").jqGrid("setGridParam", {
        search: true // 将jqGrid的search选项设为true  
    }).trigger("reloadGrid", [{ page: 1 }]);   //重新载入Grid表格，以使上述设置生效  

    $("#searchDialog").dialog("close");
}

//重置搜索条件
var clearSearch = function () {
    var sdata = {   //构建一套空的查询参数  
        searchField: "",
        searchString: "",
        searchOper: ""
    };

    var postData = $("#grid").jqGrid("getGridParam", "postData");

    $.extend(postData, sdata);  //将postData中的查询参数覆盖为空值  

    $("#grid").jqGrid("setGridParam", {
        search: false   //将jqGrid的search选项设为false  
    }).trigger("reloadGrid", [{ page: 1 }]);

    $("#searchString").val("");
};