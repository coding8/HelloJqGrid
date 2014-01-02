$(function () {
    //配置jqGrid
    jQuery("#grid").jqGrid({
        url: "/Home/GetSortingData",
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
        caption: "jqGrid data",
        loadonce: false,    //排序翻页等操作在服务端完成
        rowNum: 5,
        rowList: [5, 10, 20],  //设置分页下拉列表
        pager: "#pager",
        viewrecords: true,
        width: 500, height: "auto",
        jsonReader: { repeatitems: false }, //prmNames: { id: "No" },
        sortorder: "asc",
        sortname: "No"//传递给服务端的排序字段名
    });
    //配置对话框(使用Jquery-UI的dialog插件)  
    $("#myDialog").dialog({
        autoOpen: false,
        modal: false,    // 设置对话框为非模态对话框  
        resizable: true,
        width: 250,
        buttons: {  // 为对话框添加按钮  
            "取消": function () { $(this).dialog("close") },
            "创建": function (event) { operate(event); },//add,     
            "删除": function (event) { operate(event); },//del,     
            "更改": function (event) { operate(event); }//update    
        }
    });
    //选择日期
    $("#Birthday").datepicker();
    $("#ui-datepicker-div").css('font-size', '0.9em'); //改变大小
});

//操作对话框
function openDialog(btnName) {
    var dlg = $("#myDialog");
    var btnPane = dlg.siblings(".ui-dialog-buttonpane");

    if (btnName == "添加") {
        dlg.find("input").removeAttr("disabled").val("");
        dlg.dialog("option", "title", "创建").dialog("open");
        btnPane.find("button:not(:contains('取消'))").hide();
        btnPane.find("button:contains('创建')").show();
    }
    else if (btnName == "更改") {
        dlg.find("input").removeAttr("disabled");
        dlg.dialog('option', 'title', '更改');
        btnPane.find("button:not(:contains('取消'))").hide();
        btnPane.find("button:contains('更改')").show();
        loadSelectedRowData();//载入选中行数据
    }
    else if (btnName == "删除") {
        dlg.find("input").attr("disabled", true);
        dlg.dialog("option", "title", "删除");
        btnPane.find("button:not(:contains('取消'))").hide();
        btnPane.find("button:contains('删除')").show();
        loadSelectedRowData();//载入选中行数据
    }
}

//载入选中行数据
function loadSelectedRowData() {
    var grid = $("#grid");
    var id = grid.jqGrid("getGridParam", "selrow");
    var no = grid.jqGrid("getRowData", id).No;
    if (!id) {
        alert("请先选择要编辑的行"); return false;//退出
    } else {
        var params = { "No": no };
        //虽然用rowData可以获得各字段的值，但是这是在客户端操作；var rowData = grid.jqGrid("getRowData", id);
        //为避免数据已被改动，从数据库里读出对应编号的数据较好；
        $.ajax({
            url: '/Home/GetRowData',
            //type: 'POST',
            data: params,
            dataType: 'json',
            cache: false,
            error: function (jqXHR, textStatus, errorThrown) { alert("status:" + jqXHR.status + "\nStatusText:" + jqXHR.statusText); },
            success: function (data, textStatus, jqXHR) {//这里的data是接收服务端的数据
                var dlg = $("#myDialog");
                dlg.find("#No").val(data.No);
                dlg.find("#Name").val(data.Name);
                dlg.find("#Email").val(data.Email);

                //--日期字段需要特别处理一下
                //--return Json:显示"/Date(1387712653000)/"
                //--Newtonsoft:2013-12-22T19:12:54
                //--第一种方式：
                //--var jsonDate = data.CreatedOn;
                //--var mydate = new Date(parseInt(jsonDate.substr(6))); //结果为1387712653000 |Sun Dec 22 2013 19:38:03 GMT+0800              
                //--var a = (new Date(mydate)).toLocaleDateString() + " " + (new Date(mydate)).toLocaleTimeString();//"2013年12月22日 19:38:03"
                //--第一种方式的结果是中文：2013年12月22日 19:38:03

                //第二种方式:用Newtonsoft转为string，但是日期后有个字母T:2013-12-22T19:12:54
                var createdOn = data.CreatedOn == "" || data.CreatedOn == null ? "" : data.CreatedOn.replace("T", " ");
                dlg.find("#CreatedOn").val(createdOn);//此字段不可更改，只是显示。
                //第二种方式的结果是：2013-12-22 19:13:17

                var birthday = data.Birthday == "" || data.Birthday == null ? "" : data.Birthday.substr(0, 10);
                dlg.find("#Birthday").val(birthday);

                //在客户端更新选定数据行
                var rowData = {
                    No: data.No,
                    Name: data.Name,
                    Email: data.Email,
                    Birthday: birthday,
                    CreatedOn: createdOn
                };
                grid.jqGrid("setRowData", id, rowData);
                //打开对话框
                dlg.dialog("open");
            }
        });
    }
}

//增删改操作
function operate(event) {
    if (!$("#myForm").valid()) {    return false;    }

    var dlg = $("#myDialog");
    var grid = $("#grid");
    var id = grid.jqGrid("getGridParam", "selrow");//客户端的行号索引
    var no = grid.jqGrid("getRowData", id).No;     //选中行的No字段值

    var name = $.trim(dlg.find("#Name").val());
    var email = $.trim(dlg.find("#Email").val());
    var birthday = $.trim(dlg.find("#Birthday").val());
    var createdOn = $.trim(dlg.find("#CreatedOn").val());

    var params = {
        "No": no,
        "Name": name,
        "Email": email,
        "Birthday": birthday,
        "CreatedOn": createdOn
    };
    if ($(event.target).text() == "创建") {
        var actionUrl = "/Home/Add";
        $.ajax({
            url: actionUrl,
            type: "post",   //默认为get（此请求已被阻止，因为当用在 GET 请求中时，会将敏感信息透漏给第三方网站。若要允许 GET 请求，请将 JsonRequestBehavior 设置为 AllowGet）
            data: params,   //传递给服务端的数据
            dataType: "json",
            cache: false,
            error: function (textStatus, errorThrown) { alert("系统ajax交互错误: " + textStatus); },
            success: function (data, textStatus) {
                if (data.msg == "success") {//此处的success来源于后台的json
                    var dataRow = {     //要添加的数据
                        No: data.No,   // 从后台传来的json得到系统分配的序号（No是一个自动递增字段） 
                        Name: name,
                        Email: email,
                        Birthday: birthday,
                        CreatedOn: data.CreatedOn//接收数据库自动创建的日期
                    };
                    //在客户端操作，避免reload表格。
                    var selectedId = $("#grid").jqGrid("getGridParam", "selrow");
                    if (selectedId) {
                        $("#grid").jqGrid("addRowData", data.No, dataRow, "before", selectedId);//如果有选定行则添加在选定行前面                  
                    } else {
                        $("#grid").jqGrid("addRowData", data.No, dataRow, "first");//没有选定行则添加在第一行
                    }
                    //似乎高亮显示用户刚添加的数据行是个不错的主意
                    $("#grid").jqGrid("setSelection", data.No);
                    dlg.dialog("close");
                    alert("添加操作成功!");
                } else {
                    alert("添加操作失败!");
                }
            }
        });
    }
    else if ($(event.target).text() == "删除") {
        var actionUrl = "/Home/Delete";
        $.ajax({
            url: actionUrl,
            type: "POST",
            data: params,
            dataType: 'json',
            cache: false,
            error: function (textStatus, errorThrown) { alert("交互错误" + textStatus); },
            success: function (data, textStatus) {
                if (data.msg == "success") {
                    grid.jqGrid("delRowData", id);
                    dlg.dialog("close");
                    alert("删除成功！");
                } else {
                    alert("删除失败！");
                }
            }
        });
    }
    else if ($(event.target).text() == "更改") {
        var actionUrl = "/Home/Update";
        $.ajax({
            url: actionUrl,
            type: 'POST',
            data: params,
            dataType: 'json',
            cache: false,
            error: function (jqXHR, textStatus, errorThrown) { alert("status:" + jqXHR.status + "\nStatusText:" + jqXHR.statusText); },
            success: function (data, textStatus, jqXHR) {
                if (data.msg == "success") {
                    var rowData = {
                        No: no,
                        Name: name,
                        Email: email,
                        Birthday: birthday
                    };
                    grid.jqGrid("setRowData", id, rowData, { color: "red" });//可以添加css
                    dlg.dialog("close");
                    alert("更新成功！");
                } else {
                    alert("更新失败！");
                }
            }
        });
    }
}

//添加
var add = function () {
    if (!$("#myForm").valid()) {
        return false;
    }
    var dlg = $("#myDialog");
    var name = $.trim(dlg.find("#Name").val());
    var email = $.trim(dlg.find("#Email").val());
    var birthday = $.trim(dlg.find("#Birthday").val());

    var actionUrl = "/Home/Add";
    var params = {
        "Name": name,
        "Email": email,
        "Birthday": birthday
    };

    $.ajax({
        url: actionUrl,
        type: "post",   //默认为get（此请求已被阻止，因为当用在 GET 请求中时，会将敏感信息透漏给第三方网站。若要允许 GET 请求，请将 JsonRequestBehavior 设置为 AllowGet）
        data: params,   //传递给服务端的数据
        dataType: "json",
        cache: false,
        error: function (textStatus, errorThrown) {
            alert("系统ajax交互错误: " + textStatus);
        },
        success: function (data, textStatus) {
            if (data.msg == "success") {//此处的success来源于后台的json
                var dataRow = {     //要添加的数据
                    No: data.No,   // 从后台传来的json得到系统分配的序号（No是一个自动递增字段） 
                    Name: name,
                    Email: email,
                    Birthday: birthday,
                    CreatedOn: data.CreatedOn//接收数据库自动创建的日期
                };
                //在客户端操作，避免reload表格。
                var selectedId = $("#grid").jqGrid("getGridParam", "selrow");
                if (selectedId) {
                    $("#grid").jqGrid("addRowData", data.No, dataRow, "before", selectedId);//如果有选定行则添加在选定行前面                  
                } else {
                    $("#grid").jqGrid("addRowData", data.No, dataRow, "first");//没有选定行则添加在第一行
                }
                //似乎高亮显示用户刚添加的数据行是个不错的主意
                $("#grid").jqGrid("setSelection", data.No);
                dlg.dialog("close");
                alert("添加操作成功!");
            } else {
                alert("添加操作失败!");
            }
        }
    });
};

//删除
var del = function () {
    var dlg = $("#myDialog");
    var grid = $("#grid");
    var id = grid.jqGrid("getGridParam", "selrow");//客户端的行号索引
    var no = grid.jqGrid("getRowData", id).No;     //选中行的No字段值
    var params = {
        "No": no//模型绑定！
    };
    $.ajax({
        url: '/Home/Delete',
        type: "POST",
        data: params,
        dataType: 'json',
        cache: false,
        error: function (textStatus, errorThrown) { alert("交互错误" + textStatus); },
        success: function (data, textStatus) {
            if (data.msg == "success") {
                grid.jqGrid("delRowData", id);
                dlg.dialog("close");
                alert("删除成功！");
            } else {
                alert("删除失败！");
            }
        }
    });
}

//更改
var update = function () { //需要数据验证！
    var dlg = $("#myDialog");
    var grid = $("#grid");
    var id = grid.jqGrid("getGridParam", "selrow");//客户端的行号索引
    var no = grid.jqGrid("getRowData", id).No;     //选中行的No字段值
    var name = $.trim(dlg.find("#Name").val());
    var email = $.trim(dlg.find("#Email").val());
    var birthday = $.trim(dlg.find("#Birthday").val());
    var createdOn = $.trim(dlg.find("#CreatedOn").val());

    var params = {
        "No": no,
        "Name": name,
        "Email": email,
        "Birthday": birthday,
        "CreatedOn": createdOn
    };

    $.ajax({
        url: '/Home/Update',
        type: 'POST',
        data: params,
        dataType: 'json',
        cache: false,
        error: function (jqXHR, textStatus, errorThrown) { alert("status:" + jqXHR.status + "\nStatusText:" + jqXHR.statusText); },
        success: function (data, textStatus, jqXHR) {
            if (data.msg == "success") {
                var rowData = {
                    No: no,
                    Name: name,
                    Email: email,
                    Birthday: birthday
                };
                grid.jqGrid("setRowData", id, rowData, { color: "red" });//可以添加css
                dlg.dialog("close");
                alert("更新成功！");
            } else {
                alert("更新失败！");
            }
        }
    });
};