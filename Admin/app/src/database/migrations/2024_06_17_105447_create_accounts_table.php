<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration {
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        // ::table はテーブルの修正 , ::create はテーブルの作成

        // テーブルの構成を作成
        Schema::create('accounts', function (Blueprint $table) {
            $table->id();
            $table->string('name', 20);     // 文字数制限を指定しない場合は255文字になる
            $table->string('password', '100');
            $table->timestamps();   // created_atとupdated_atが入る

            // ユニーク制約設定
            $table->unique('name');
        });
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        // テーブル削除時の処理
        Schema::dropIfExists('accounts');

        // dropIfExists ... 存在していたら削除
        // drop ... 削除
    }
};
