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
            { label: 'Age',name:'Age',index:'Age'},
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
        caption: "jqGrid Multiple Search",
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

function multipleSearch() {
    var rules = "";

    $("#multipleSearch tbody tr").each(function (i) {    //从multipleSearch中找到各个查询条件行  
        var searchField = $(".searchField", this).val(); //获得查询字段  
        var searchOper = $(".searchOper", this).val();   //获得查询方式  
        var searchString = $(".searchString", this).val();  //获得查询值  

        if (searchField && searchOper && searchString) { //如果三者皆有值且长度大于0，则将查询条件加入rules字符串  
            rules += ',{"field":"' + searchField + '","op":"' + searchOper + '","data":"' + searchString + '"}';
        }
    });

    if (rules) { //如果rules不为空，且长度大于0，则去掉开头的逗号  
        rules = rules.substring(1);
    }

    //串联好filtersStr字符串  
    var filtersStr = '{"groupOp":"AND","rules":[' + rules + ']}';

    var postData = $("#grid").jqGrid("getGridParam", "postData");

    //将filters参数串加入postData选项  
    $.extend(postData, { filters: filtersStr });

    $("#grid").jqGrid("setGridParam", {
        search: true    //将jqGrid的search选项设为true  
    }).trigger("reloadGrid", [{ page: 1 }]);   
};

//重置搜索条件
var clearSearch = function () {
    var filtersStr = '';
    var postData = $("#grid").jqGrid("getGridParam", "postData");
    $.extend(postData, { filters: filtersStr });
    $("#grid").jqGrid("setGridParam", {
        search: false   //将jqGrid的search选项设为false  
    }).trigger("reloadGrid", [{ page: 1 }]);

    $(".searchString").val("");
};