<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration {
    public function up(): void
    {
        Schema::create('result', function (Blueprint $table) {
            $table->id();
            $table->integer('user_id');         // ユーザーID
            $table->boolean('is_winner');       // 勝ったかどうか
            $table->integer('point');           // 獲得ポイント
            $table->integer('character_id');    // 使用したキャラクターID
            $table->timestamps();

            $table->unique('name');
        });
    }

    public function down(): void
    {
        Schema::table('', function (Blueprint $table) {
            //
        });
    }
};
